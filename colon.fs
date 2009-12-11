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

\ The colon-definition compiler

\ ---------- Colon-definition words ----------

\ Compile the top of the stack as a literal
: LITERAL \ ( n -- )
    ['] (LITERAL) XTCOMPILE,
    COMPILE, ; IMMEDIATE

\ Compile the address on the top of the stack as a literal
: ALITERAL \ ( addr -- )
    ['] (LITERAL) XTCOMPILE,
    ACOMPILE, ; IMMEDIATE

\ Compile the xt on the top of the stack as a literal
: XTLITERAL \ ( xt -- )
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
    THEN ; IMMEDIATE

\ Enter interpretation state
: [ \ ( -- )
    INTERPRETATION-STATE STATE ! ;

\ Enter compilation state
: ] \ ( -- )
    COMPILATION-STATE STATE ! ;

\ Complete a colon-definition
: ; \ ( xt -- )
    0 COMPILE, \ NEXT
    END_DEFINITION
    [ DROP ; IMMEDIATE

\ The colon-definer
: : \ ( "name" -- xt )
    START-DEFINITION
    PARSE-WORD [ ' (:) CFA@ ] (HEADER,)
    ] ;
