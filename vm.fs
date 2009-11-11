\ $Id: vm.fs,v 1.5 2007/06/13 15:57:39 sd Exp $

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

\ Status masks
: IMMEDIATE-MASK    1 ;
: REDIRECTABLE-MASK 2 ;

\ States
: INTERPRETATION-STATE 0 ;
: COMPILATION-STATE 1 ;

\ User variables
0 USER EXECUTIVE             \ xt of outer executive (OUTER by default)
1 USER STATE                 \ interpreting or compiling
2 USER BASE                  \ number base
3 USER INPUTSOURCE           \ pointer to internal structure for input source
4 USER (CURRENT-VOCABULARY)  \ current vocabulary
5 USER TIB                   \ address of the TIB
6 USER >IN                   \ input offset

\ ---------- Truth values ----------

1 CONSTANT TRUE
0 CONSTANT FALSE


\ ---------- Standard number bases ----------

: BINARY   2 BASE ! ;
: DECIMAL 10 BASE ! ;
: HEX     16 BASE ! ;

DECIMAL

