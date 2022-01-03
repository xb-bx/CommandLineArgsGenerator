$scriptblock = {
	param($wordToComplete, $commandAst, $cursorPosition)
	(Converter complete $cursorPosition $commandAst).Split("\n") | ForEach-Object {
		[System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
	}
}
Register-ArgumentCompleter -Native -CommandName Converter -ScriptBlock $scriptblock