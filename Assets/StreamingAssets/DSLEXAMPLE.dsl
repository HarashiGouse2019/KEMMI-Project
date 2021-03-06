### TITLE
This is going to be the title (obviously).
This is just your normal everyday comment.
###
TITLE: "EXAMPLE TEXT"

AUTHOR: "MIIJII"

DATE: "March 24, 2020"

###This is how you define a dialogue set.###
<DIALOGUE_SET_000>

NOTE: 
### BASICS

You always start a line with '@' and always end with '<<'.
You can stylize your dialogue with [BOLD], [UNDERLINE], and [ITALIZE].
To end styling, you add '::END' after.

For example: 

	@ [BOLD] Hello! [BOLD::END] <<
	@ [ITALIZE] Hello! [ITALIZE::END] <<
	@ [UNDERLINE] Hello! [UNDERLINE::END] <<
	
You can also change texting speed with the [SPEED] command. Here are 
the 7 available speeds:

	SLOWEST
	SLOWER
	SLOW
	NORMAL
	FAST
	FASTER
	FASTEST

###
@ [BOLD] [SPEED::FASTER] OH MY GOSH!!!! [BOLD::END] <<

### LEAVE A SPACE
Be sure that use [], you level space after the commands. 
If not, the command is ignored.
###
@ [SPEED::FAST] The Dialogue Scripting Language (DSL) is actually working.<<

@ [SPEED::NORMAL] It's at a basic stage, you can only [BOLD] BOLD[BOLD::END] ,[ITALIZE] ITALIZE[ITALIZE::END] , [UNDERLINE] UNDERLINE[UNDERLINE::END] , and change the text speed.<<

@ There are other commands that needs to be implemented, such as SOUND and CUE.<<

@ But SPEED, BOLD, ITALIZE, and UNDERLINE are the pretty important stuff.<<

### NESTING
You can nest commands as well, as you see for this one!
And you can change the speed more than 1 time!
###
@ I don't want to get [BOLD] [ITALIZE] [SPEED::SLOWER] too complicated[BOLD::END] [ITALIZE::END] ; [SPEED::FAST] I just wanted to have a nicer way to write dialogue for a game.<<

@ [SPEED::SLOWEST] ...<<

@ [SPEED::FASTER] Yo!<<

### CONSISTENT SPEED
When the texting speed changes, 
it remains constant until it's changed again.
###
@ [BOLD] BRO!!! [BOLD::END] <<

@ [BOLD] [UNDERLINE] THE SCRIPTING LANGUAGE IS ACTUALLY WORKING!!! [UNDERLINE::END] [BOLD::END] <<

@ [BOLD] AND WHAT'S COOL ABOUT THE [SPEED::SLOWER] SPEED...<<

@ [SPEED::FASTEST] IT CARRIES ON WITH OTHER DIALOGUE UNLESS I CHANGE IT ON A DIFFERENT LINE!!!<<

@ [SPEED::NORMAL] Like [SPEED::SLOWEST] ...[SPEED::FASTER] RIGHT NOW!!! [SPEED::SLOWEST]  [SPEED::FASTER] IT'S WORKING!!!!!!<<

@ [SPEED::FAST] This is so cool XD I'm so happy!!!<<

@ [SPEED::NORMAL] We will also test another function...<<

@ And that is the [BOLD] ACTION [BOLD::END] function.<<

@ We should two astericks surrounding a word(s). For example: *Sigh*.<<

@ Are you ready???<<

@ [ACTION::"Sigh"] <<

@ [BOLD] [SPEED::FASTEST] [ACTION::"Jumps with Joy"] IT WORKS!!!<<

### END MARKER
When you are complete with a dialogue set,
you include the <END> tag.
###
<END>

###CONCLUSION
A good practice to do for using DSL for Unity,
stylize first before you do any commands.

For example, type:

	[BOLD] [SPEED::FASTER] Hi![BOLD::END]

instead of:

	[SPEED::FASTER] [BOLD] Hi! [BOLD::END]
	
I assure you, if you did it the wrong way, you would have a of issues... 

Another good practice is when you are doing consecutive actions, leave spaces on both sides

For example:
	
	_ represents a space
	
	_[BOLD]__[SPEED::FASTEST]__[EXPRESSION::1]__[HALT::100]__[ACTION::"Sigh"]_
	
instead of
	_[BOLD]_[SPEED::FASTEST]__EXPRESSION::1]_[HALT::100]_[ACTION::"Sigh"]_
	
If your dialogue appears to look funky when running, please consider doing the first example for functions
that seem like are not working properly.
###
