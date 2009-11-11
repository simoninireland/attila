\ $Id: case.fs,v 1.3 2007/05/16 13:41:03 sd Exp $

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

\ Case structures
\
\ A CASE ... OF ... ENDOF ... ENDCASE structure is essentially a nested
\ IF conditional that tests repeatedly for equality against each value
\ provided before each OF in sequence, running the first successful match
\ and exiting. There is no default behaviour in the case of no match: the
\ construct will simply drop through (although DUP will of course cause
\ an instant match).

\ Start the construction
: CASE 0 (CS-START) ; IMMEDIATE

\ Check the two top values, continue if equal and jump if not
: OF \ ( v1 v2 -- | v1 )
    [COMPILE] OVER [COMPILE] =
    [COMPILE] (?BRANCH) TOP 0 COMPILE,
    [COMPILE] DROP ; IMMEDIATE

\ Jump out to the end of the structure, and re-direct the last jump to here
: ENDOF \ ( -- )
    [COMPILE] (BRANCH) >END
    DUP JUMP> SWAP ! ; IMMEDIATE

\ End the structure
: ENDCASE \ ( v1 -- )
    [COMPILE] DROP (CS-END) ; IMMEDIATE
    
    