\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007--2011, Simon Dobson <simon.dobson@computer.org>.
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

\ The standard outer executive
\
\ Unlike the traditional outer executive, Attila's version is modularised
\ to allow code to try to parse the lexeme in the input stream. The default version
\ tests for words (executed or compiled depending on the interpretation state
\ and the word's immediacy) and numbers (stacked or compiled as literals).
\ Adding a new lexeme type such as floats requires defining a hook word and hanging
\ it on the PARSE-WORD-HOOK. Such parsers need to be written carefully with
\ respect to their stack effects, as they're running within a larger loop.
\ If running the hook gets to the end without an early exit, the lexeme
\ is considered unparsed.
\
\ sd: handling strings here would be good.....


\ Hook for word parsers
HOOK PARSE-WORD-HOOK


\ ---------- Parsing components----------
\ Parsing hooks must all have the stack comment ( addr n -- -1 | addr n 0 )
\ They take a lexeme as a string and either process it -- completing the
\ hook and returnig TRUE -- or don't in which case they leave it on the
\ stack and add FALSE

\ Words, compile or execute depending on mode and immediacy
:NONAME ( addr n -- -1 | addr n 0 )
    2DUP FIND ?DUP IF
	2SWAP 2DROP
	0< INTERPRETING? OR IF
	    EXECUTE
	ELSE
	    CTCOMPILE,
	THEN
	TRUE
    ELSE
	FALSE
    THEN ;
HANG-ON PARSE-WORD-HOOK

\ Numbers, left on stack or compiled as literals
:NONAME ( addr n -- -1 | addr n 0 )
    2DUP NUMBER? IF
	ROT 2DROP
	INTERPRETING? NOT IF
	    POSTPONE LITERAL
	THEN
	TRUE
    ELSE
	FALSE
    THEN ;
HANG-ON PARSE-WORD-HOOK


\ ---------- The executive ----------

\ The interactive outer executive reads words while they're available
\ and processes them using the parsing hook
: OUTER \ ( -- )
    BEGIN
	PARSE-WORD ?DUP 0= IF
	    EXIT
	ELSE
	    PARSE-WORD-HOOK RUN-HOOK NOT IF
		TYPE S" ?" ABORT
	    THEN
	THEN
    AGAIN ;
