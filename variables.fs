\ $Id: variables.fs,v 1.6 2007/05/25 13:13:27 sd Exp $

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

\ Data blocks, variables, user variables, constants and values

\ Create a variable holding a single-precision cell
: VARIABLE \ ( "name" -- )
    DATA 0 , ;
    
\ Create a real constant, which substitutes its value immediately
\ as a literal
: CONSTANT \ ( v "name" -- )
    CREATE IMMEDIATE ,
  DOES> @
    INTERPRETING? NOT
    [ ' (?BRANCH) COMPILE, TOP 0 COMPILE, ]   \ IF
        POSTPONE LITERAL
    [ DUP JUMP> SWAP ! ] ;                    \ THEN
 
\ Create a value, which returns its value when executed
: VALUE \ ( v "name" -- )
    CREATE ,
  DOES> @ ;

\ Re-assign a value
' !
:NONAME POSTPONE LITERAL [COMPILE] ! ;
INTERPRET/COMPILE (TO)

\ Top-level TO
: TO \ ( v "name" -- )
    ' >BODY POSTPONE (TO) ; IMMEDIATE

\ Create a user variable, returning the address of an indexed location
\ within the user area
: USER \ ( n "name" -- )
    CREATE ,
    DOES> @ USERVAR ;
  
  