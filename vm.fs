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

\ Virtual machine constants
\
\ This file is pre-processed to generate the necessary
\ definitions, typically in C. It should contain only
\ simple code such as CONSTANT and USER declarations

\ Status masks
1 CONSTANT IMMEDIATE-MASK
2 CONSTANT REDIRECTABLE-MASK

\ States
0 CONSTANT INTERPRETATION-STATE
1 CONSTANT COMPILATION-STATE

\ User variables
 0 USER COLDSTART             \ xt of the word that cold-starts the system
 1 USER EXECUTIVE             \ xt of outer executive
 2 USER CODETOP               \ first free code address
 3 USER DATATOP               \ first free data address
 4 USER STATE                 \ compiler state
 5 USER BASE                  \ number base
 6 USER INPUTSOURCE           \ pointer to internal structure for terminal input
 7 USER OUTPUTSINK            \ pointer to internal structure for terminal output
 8 USER >IN                   \ input offset
 9 USER TIB                   \ address of the TIB
10 USER LAST                  \ xt of the most-recently-defined word
11 USER TRACE                 \ trace (debugging) flag
12 USER CURRENT               \ the current wordlist receiving definitions

\ Truth values
0        CONSTANT FALSE
0 INVERT CONSTANT TRUE

