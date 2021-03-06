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
    POSTPONE OVER POSTPONE =
    POSTPONE (?BRANCH) JUMP-FORWARD
    POSTPONE DROP ; IMMEDIATE

\ Jump out to the end of the structure, and resolves the jump from the
\ previous case to to here
: ENDOF \ ( -- )
    POSTPONE (BRANCH) >END
    JUMP-HERE ; IMMEDIATE

\ End the structure
: ENDCASE \ ( v1 -- )
    POSTPONE DROP ENDS> (CS-END) ; IMMEDIATE
    
    