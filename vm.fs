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

\ Virtual machine description
\
\ This file is pre-processed to generate the necessary
\ definitions, typically in C. It should contain only
\ simple code such as CONSTANT and USER declarations

\ Sizes
24 1024 * CONSTANT IMAGE-SIZE           \ initial image size in cells
50        CONSTANT DATA-STACK-SIZE      \ data stack size in cells
50        CONSTANT RETURN-STACK-SIZE    \ return stack size in cells
25        CONSTANT USER-SIZE            \ number of user variables allowed
256       CONSTANT TIB-SIZE             \ terminal input buffer size

\ Status masks
1 CONSTANT IMMEDIATE-MASK          \ IMMEDIATE words, executed even when compiling
2 CONSTANT REDIRECTABLE-MASK       \ REDIRECTABLE words, amenable to DOES>
4 CONSTANT HIDDEN-MASK             \ hidden words that FIND won't find

\ States
0 CONSTANT INTERPRETATION-STATE    \ interpretation state
1 CONSTANT COMPILATION-STATE       \ (standard) compilation state

\ User variables
 0 USER COLDSTART             \ xt of the word that cold-starts the system
 1 USER EXECUTIVE             \ xt of outer executive
 2 USER (TOP)                 \ first free code address
\  USER (CEILING)             \ highest code address currently available
 3 USER (HERE)                \ first free data address
\  USER (THERE)               \ highest data address currently available
 4 USER STATE                 \ compiler state
 5 USER BASE                  \ number base
 6 USER INPUTSOURCE           \ pointer to internal structure for input
 7 USER OUTPUTSINK            \ pointer to internal structure for output
 8 USER >IN                   \ input offset
 9 USER TIB                   \ address of the TIB
10 USER LAST                  \ xt of the most-recently-defined word
11 USER TRACE                 \ trace (debugging) flag
12 USER CURRENT               \ the current wordlist receiving definitions
13 USER (DATA-STACK)
14 USER (RETURN-STACK)

\ Truth values
0        CONSTANT FALSE
0 INVERT CONSTANT TRUE

