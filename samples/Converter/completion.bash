#!/usr/bin/env bash
_converter_completions()
{  
	echo -ne "\033[6n"            # A bunch of some bash stuff
	read -s -d";" garbage          
	read -s -d R pos 
	pos=$(($pos - 2)) 
 	COMPREPLY=($(compgen -W "$(Converter complete $pos $COMP_LINE)" ""))
}

complete -F _converter_completions Converter
