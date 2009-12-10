\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
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

\ The initial dictionary containing the words necessary to bring up
\ the colon-definiton compiler.

\ ---------- Error handling ----------

\ Abort with an error, re-starting the executive
: ABORT \ ( addr len -- )
    TYPE
    SPACE TYPE
    WARM ;


\ ---------- Compilation helpers ----------

\ Test whether we're interpreting
: INTERPRETING? \ ( -- f )
    STATE @ INTERPRETING = ;

\ Find the xt of the next word in the input source, or abort
: ' \ ( "word" -- xt )
    PARSE-WORD 2DUP FIND
    ?DUP IF
	ROT 2DROP
    ELSE
	TYPE
	S" ?" ABORT
    THEN ;


\ ---------- Colon-definition words ----------

\ Compile the top of the stack as a literal
: LITERAL \ ( n -- )
    ['] (LITERAL) XTCOMPILE,
    XTCOMPILE, ; IMMEDIATE

\ Compile the string on the tp of the stack as a literal
: SLITERAL \ ( addr len -- )
    ['] (SLITERAL) XTCOMPILE,
    SCOMPILE, ; IMMEDIATE

\ Compile a "-delimited string from the input source as a literal
: S" \ ( "string" -- )
    32 CONSUME \ spaces
    [CHAR] " PARSE
    ?DUP IF
	SLITERAL
    ELSE
	S" String not delimited" ABORT
    THEN ;

\ Enter interpretation state
: [ \ ( -- )
    INTERPRETATION-STATE STATE ! ;

\ Enter compilation state
: ] \ ( -- )
    COMPILATION-STATE STATE ! ;

\ The colon-definer
: : \ ( "name" -- )
    START-DEFINITION
    PARSE-WORD [ ' (:) CFA@ ] (HEADER,)
    ] ;

\ Complete a colon-definition
: ; \ ( -- )
    0 COMPILE, \ NEXT
    END_DEFINITION
    [ DROP ; IMMEDIATE


\ ---------- The outer interpreter ----------

\ The outer executive
: OUTER \ ( -- )
    BEGIN
	PARSE-WORD ?DUP NOT IF
	    2DUP FIND ?DUP IF
		ROT 2DROP INTERPRETING?
		OVER IMMEDIATE? OR IF
		    EXECUTE
		ELSE
		    COMPILE,
		THEN
	    ELSE
		2DUP NUMBER? IF
		    ROT 2DROP INTERPRETING? NOT IF
			LITERAL
		    THEN
		ELSE
		    TYPE S" ?" ABORT
		THEN
	    THEN
	THEN
    EXHAUSTED? UNTIL ;


