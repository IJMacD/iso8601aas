#!/bin/bash

set -o errexit
set -o nounset
set -o pipefail

if [ -n "$(git status --porcelain)" ]; then
  echo "Please ensure there are no changes or untracked files before rebuilding"
  exit 1
fi

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
source ${SCRIPT_DIR}/vars.sh

[[ ! -z $(k3d cluster list ${APPNAME} | grep '0/1') ]] && k3d cluster stop --all && k3d cluster start ${APPNAME}

# Delete the images on the node - not the registry!
docker exec k3d-${APPNAME}-server-0 sh -c 'ctr image rm $(ctr image list -q)'

docker build ${SCRIPT_DIR}/ -f ${SCRIPT_DIR}/Dockerfile \
  -t ${REGISTRY_NAME}/${REPO}/${APPNAME}:${GIT_TAG} -t ${LOCAL_REGISTRY}/${REPO}/${APPNAME}:${GIT_TAG}
docker push ${LOCAL_REGISTRY}/${REPO}/${APPNAME}:${GIT_TAG}

# helm dependency build $SCRIPT_DIR/kube/chart/${APPNAME}/

helm upgrade --install ${APPNAME} \
  $SCRIPT_DIR/kube/chart/${APPNAME}/ \
  --namespace ${APPNAME} --create-namespace \
  -f ${SCRIPT_DIR}/kube/chart/${APPNAME}/overrides.dev.yaml \
  --set web.repository.tag=${GIT_TAG}