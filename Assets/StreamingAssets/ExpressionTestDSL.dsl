TITLE: "Expression Test"

AUTHOR: "MIIJII"

DATE: "March 25, 2020"

																												###
																												
																						Expressions is something that's really cool.

																			You can define an expression underneath the <EXPRESSION> tag.

																					The syntax for an expression is as follows:
																					
	
																						  (Character_Name)_EXPRESSION_(STATE)
	
	
	
														If you have more than one state or emotion, add a "-" and the number (it could be zero-based or not).


																									JOHN_EXPRESSION_HAPPY-0 = 0
																									JOHN_EXPRESSION_HAPPY-1 = 1
																									
																									
																												###

<EXPRESSIONS>
KEMMI_EXPRESSION_HAPPY-0 = 1
KEMMI_EXPRESSION_HAPPY-1 = 2
KEMMI_EXPRESSION_SURPRISED = 3
KEMMI_EXPRESSION_NEUTRAL = 4
KEMMI_EXPRESSION_MAD-0 = 5
KEMMI_EXPRESSION_AMAZED = 6
KEMMI_EXPRESSION_SMUG = 7
KEMMI_EXPRESSION_HAPPY-3 = 8
KEMMI_EXPRESSION_HAPPY-4 = 9
KEMMI_EXPRESSION_MAD-1 = 10

<POSES>

<DIALOGUE_SET_000>

																											###
																											 
																			You would call a EXPRESSION function by the expression name 


																							[EXPRESSION::JOHN_EXPRESSION_HAPPY-0] 
																							

																											###
																											
@ [SPEED::NORMAL][HALT::2000]Sooooooooo[SPEED::SLOWEST][HALT::2000]... [ACTION::"CAHHHHHHHHH"] [HALT::1000][ACTION::"SWOSH"] [HALT::1000][ACTION::"SWUSH"] [HALT::1000][ACTION::"SMASHING"] <<
																											
@ [HALT::1000][ACTION::"Sigh"] [SPEED::NORMAL][HALT::1000]Let's get the bread, [EXPRESSION::1][HALT::5000]bread has [HALT::500]been [HALT::500]baked. [EXPRESSION::2] <<

@ [EXPRESSION::9] Sounds awesome doesn't it. [INPUT::0]

<END>
