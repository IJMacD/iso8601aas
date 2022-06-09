grep -v '^#' | while read -r format;
    do echo -n $format " ";
    format="$(echo -n $format | jq -sRr @uri)";
    OUTPUT="$(curl -s "http://localhost:5000/$format")";
    if echo $OUTPUT | grep -q 'error';
        then echo -e "\e[31mError\e[0m";
    elif echo $OUTPUT | grep -q 'type';
        then echo -e "\e[32mOK\e[0m";
    else
        echo "Unknown";
    fi;
done < "${1:-/dev/stdin}"