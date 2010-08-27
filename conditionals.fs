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

\ Conditionals
\
\ These immediate words construct conditionals and loops using
\ the primitive (BRANCH) and (?BRANCH) words.
\
\ Note that these words are all IMMEDIATE. Note further that conditionals
\ do *not* count as control structures, and so LEAVE and related words refer
\ to any containing loop

\ Test the top of the stack and branch if false
: IF \ ( -- addr )
    [COMPILE] (?BRANCH) JUMP-FORWARD ; IMMEDIATE

\ Jump out and resolve the previous IF branch
: ELSE \ ( addr -- addr' )
    [COMPILE] (BRANCH) JUMP-FORWARD
    SWAP JUMP-HERE ; IMMEDIATE

\ Complete the conditional by resolving the last branch
: THEN \ ( addr -- )
    JUMP-HERE ; IMMEDIATE
