\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007--2009, Simon Dobson <simon.dobson@computer.org>
\ All rights reserved.
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

\ The cross-compiler for "ordinary" words
\
\ Primitives are compiled into the target image, with locator words
\ placed into TARGET that, when executed, return the target xt. 

\ ---------- Constants and data structures ----------

\ Create a locator for a word
: (LOCATOR) \ ( txt addr n -- )
    (DATA)
    , ;
: LOCATOR \ ( "name" -- )
    PARSE-WORD (LOCATOR) ;


\ ---------- Compiler ----------

\ Manipulate cross-compiler state
: [ INTERPRETATION-STATE STATE ! ; IMMEDIATE
: [ COMPILATION-STATE STATE ! ; IMMEDIATE

\ Compile a word into the target image, with a locator in TARGET
\ sd: Would it be easier to change the executive?
: : \ ( "name" -- xt )
    PARSE-WORD 2DUP
\    ['TARGET] (:) CFA@        \ tcf for (:) in the target
\    (HEADER,)                 \ returns txt of word 
\    ROT (LOCATOR)             \ ...which then ends up in the locator

    POSTPONE [
    BEGIN
	PARSE-WORD ?DUP 0<> IF
	    STATE @ COMPILATION-STATE = IF
		\ target-compiling
		2DUP S" ;" S= IF
		    \ end of the word
		    2DROP LEAVE
		ELSE
		    2DUP TARGET-WORDLIST FIND-IN-WORDLIST IF
			\ we've found a TARGET locator, so
			\ execute it to get the txt and then
			\ compile that into the target image
			EXECUTE @
			XTCOMPILE,
			2DROP
		    ELSE
 			\ look for an IMMEDIATE word in CROSS
			2DUP CROSS-WORDLIST FIND-IN-WORDLIST 1 NEGATE = IF
			    \ got an IMMEDIATE CROSS word, execute it
			    EXECUTE
			    2DROP
			ELSE
			    \ no valid word, die
			    TYPE S" ? when compiling" ABORT
			THEN
		    THEN
		THEN
	    ELSE
		\ interpreting
		2DUP CROSS-WORDLIST FIND-IN-WORDLIST IF
		    \ got a word in CROSS, execute it
		    EXECUTE 2DROP
		ELSE
		    \ no word we can execute die
		    TYPE S" ? when interpreting" ABORT
		THEN
	    THEN
	THEN
	EXHAUSTED?
    UNTIL
    POSTPONE ] ;


			