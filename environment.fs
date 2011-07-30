\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007--2010, Simon Dobson <simon.dobson@computer.org>.
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

\ Environmental queries
\
\ These allow a program to query its own execution environment -- which
\ is to be discouraged, but is occasionally useful. In the main this
\ re-presents information, in the format required by the Forth standard,
\ that Attila retains anyway as system variables and the like.
\ Requires: wordlists.fs

\ The namespace for environmental query words
WORDLIST CONSTANT ENVIRONMENT-DESCRIPTION-WORDLIST 
ENVIRONMENT-DESCRIPTION-WORDLIST PARSE-WORD ENVIRONMENT-DESCRIPTION (VOCABULARY)

\ Query the environment
: ENVIRONMENT? ( addr n -- v -1 | 0 )
    ENVIRONMENT-DESCRIPTION-WORDLIST SEARCH-WORDLIST IF
	\ found query, execute it to produce the result
	EXECUTE TRUE
    ELSE
	\ undefined
	FALSE
    THEN ;

\ ---------- The environment ----------

\ Define the queries using normal words in a known namespace
<WORDLISTS ONLY FORTH ALSO ENVIRONMENT-DESCRIPTION ALSO DEFINITIONS

\ Maximum size in bytes of counted strings
: /COUNTED-STRING [ /CHAR 8 * ] LITERAL ;

\ Size in bytes of the hold area
: /HOLD
    [ S" SCRATCH-SIZE" FIND ] [IF]
	[ EXECUTE ] LITERAL
    [ELSE]
	0
    [THEN] ;

\ Size in bytes of the scratch area
: /PAD
    [ S" SCRATCH-SIZE" FIND ] [IF]
	[ EXECUTE ] LITERAL
    [ELSE]
	0
    [THEN] ;

\ Bits per address unit
: ADDRESS-UNIT-BITS 8 ;

\ Do we have the CORE words?
: CORE TRUE ;

\ Do we have the CORE-EXT words?
: CORE-EXT TRUE ;

\ Do we use floored division?
: FLOORED TRUE ;

\ Maximum character
: MAX-CHAR 256 ;

\ Size in cells of the return stack
: RETURN-STACK-CELLS RETURN-STACK-SIZE ;

\ Size in cells of the data stack
: STACK-CELLS DATA-STACK-SIZE ;

WORDLISTS>
