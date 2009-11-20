\ $Id$

\ This file is part of Attila, a minimal threaded interpretive language
\ Copyright (c) 2007, UCD Dublin. All rights reserved.
\
\ Attila is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.
\
\ Attila is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.
\
\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

\ Stack-comment-style local variable definitions
\
\ A locals definition that minimcs stack comments, in the style of Gforth's
\ locals (although not *quite* as flexible).
\
\ The esiest way to explain is by an example: consider the definition
\
\ : POINTLESS { a b -- c }
\     a b + TO c ;
\
\ The part in curly brackets looks like a stack comment, but actually introduces
\ local values. The declarations before the -- are arguments, which are
\ popped off the data stack in the order you would expect. These are then
\ available as VALUEs within the body of the word. The declarations after the
\ -- are *also* available as values, and will be pushed back onto the
\ data stack at the end of the word. Hence the assignment in the body of
\ POINTLESS will ass a to b and put the result into c, which will then
\ land on the stack in the way suggested by the comment.
\
\ It's safe to use the same symbol on both sides of the --, so the definition
\
\ : DUP { a -- a a } ;
\
\ will do exactly what it looks like, rather less efficiently than the built-in
\ version.
\ Requires stacks.fs, locals-base.fs


\ The return values stack
#LOCALS/WORD STACK RETURN-LOCALS-STACK

\ Create result and argument locals. The result is a list of local definition
\ xts on the stack topped with the count of the number of arguments and the
\ total number of locals
: (CREATE-RESULT-LOCALS) \ ( na -- ... na n )
    DUP BEGIN
	PARSE-WORD
	2DUP S" }" S= IF
	    2DROP FALSE
	ELSE
	    TRUE
	THEN
    WHILE
	    2DUP (LOCAL?) ?DUP IF
		RETURN-LOCALS-STACK >ST
		2DROP
	    ELSE
		(CREATE-LOCAL)
		DUP RETURN-LOCALS-STACK >ST
		ROT 1+
	    THEN
    REPEAT ;
: (CREATE-LOCALS) \ ( -- ... na n )
    0 BEGIN
	PARSE-WORD
	2DUP S" --" S= IF
	    2DROP (CREATE-RESULT-LOCALS)
	    FALSE
	ELSE
	    2DUP S" }" S= IF
		2DROP DUP FALSE
	    ELSE
		TRUE
	    THEN
	THEN
    WHILE
	    2DUP (LOCAL?) IF
		S" Duplicate argument" ABORT
	    THEN
	    (CREATE-LOCAL) SWAP 1+
    REPEAT ;

\ Create the placeholders for the return values
: >LOCAL-RETURNS \ ( n -- )
    0 DO
	0 >LOCAL
    LOOP ;

\ Create the code to push the results onto the stack
: (CREATE-RESULTS) \ ( -- )
    RETURN-LOCALS-STACK #ST 0<> IF
	RETURN-LOCALS-STACK ST>
	RECURSE
	EXECUTE
    THEN ;
    
\ Override ; in the LOCALS vocabulary, having it compile the code
\ to off-load the values on the locals stack
\ sd: This is a horrible hack and completely non-standard as it
\ doesn't work with exceptions etc -- it badly needs re-writing
CURRENT
ALSO LOCALS ALSO DEFINITIONS
: ;
    (CREATE-RESULTS)                       \ push the results, if any
    LOCALS-MARKER @ EXECUTE                \ forget the local symbols
    PREVIOUS                               \ into previous environment
    [COMPILE] LOCALS>                      \ compile code to clean up locals stack
    [ PREVIOUS ] POSTPONE ; ; IMMEDIATE    \ do the normal ; behaviour
(DEFINITIONS)

\ Parse a list of names, creating locals for each.
: {
    \ prepare
    (CREATE-LOCALS-MARKER)
    RETURN-LOCALS-STACK ST-CLEAR
    
    \ create argument and return locals
    (CREATE-LOCALS)
    
    \ patch each local with its run-time offset on the locals stack
    SWAP >R DUP 0> IF
	DUP 0 DO
	    DUP I - -ROT >BODY !
	LOOP
    THEN
    R>
    
    \ leave the locals vocabulary on the top of the search order, restoring
    \ the previous current vocabulary
    -ROT (DEFINITIONS)

    \ compile the code to set up the locals at run-time
    \ tot args
    [COMPILE] START-LOCALS
    SWAP OVER -   \ args ret
    ?DUP 0> IF
	POSTPONE LITERAL [COMPILE] >LOCAL-RETURNS
    THEN
    ?DUP 0> IF
	POSTPONE LITERAL [COMPILE] >LOCALS
    THEN ; IMMEDIATE

