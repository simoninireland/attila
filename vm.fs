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

\ Virtual machine constants and other bits
\
\ It's vitally important these match the definitions in the underlying
\ C code of the virtual machine -- so important, in fact, that this file
\ should really be derived automatically...
\
\ There are some overlaps that need to be sorted out, for example the inclusion
\ of user variables that are irrelevant in embedded systems with no interaction.

\ Status masks
1 CONSTANT IMMEDIATE-MASK
2 CONSTANT REDIRECTABLE-MASK

\ States
0 CONSTANT INTERPRETATION-STATE
1 CONSTANT COMPILATION-STATE

\ User variables
0 USER COLDSTART             \ xt of the word that cold-starts the system
1 USER EXECUTIVE             \ xt of outer executive (OUTER by default)
2 USER STATE                 \ interpreting or compiling
3 USER BASE                  \ number base
4 USER INPUTSOURCE           \ pointer to internal structure for input source
5 USER >IN                   \ input offset
6 USER TIB                   \ address of the TIB
7 USER LAST                  \ xt of the most-recently-defined word
8 USER TRACE                 \ trace (debugging) flag

\ ---------- Truth values ----------

0            CONSTANT FALSE
FALSE INVERT CONSTANT TRUE


\ ---------- Standard number bases ----------
\ Note: by convention the system is in decimal at start-up

: BINARY   2 BASE ! ;
: DECIMAL 10 BASE ! ;
: HEX     16 BASE ! ;

