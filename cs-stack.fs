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

\ The control structures stack
\
\ Attila implements loops by using IMMEDIATE words to construct
\ a more basic branching structure. While this is sufficient to provide
\ structures with a single entry and exit point, some operations
\ can really only be programmed effectively with escapes.
\
\ The control stack keep track of the compile-time structure of
\ control structures being defined. This retains enough information to
\ construct early exits. Note that it remains the responsibility of the
\ loop code to manage any values placed on the return stack under
\ normal circumstances: the control stack value is used only to unwind
\ in case of exceptions.
\ Requires: stacks.fs

\ ---------- The control structures and leaves stack ----------

\ The maximum compile-time depth of loops allowed -- set with care!
5 VALUE MAX-LOOP-DEPTH

\ The maximum number of pending escapes allowed from all loops -- set with care!
5 MAX-LOOP-DEPTH * VALUE MAX-LEAVES

\ The number of cells per control structure. Changing this will require
\ re-coding the (CS-*) words. Currently we maintain:
\    * the start (repetition) address of the construct
\    * the number of elements the construct keeps on the return
\      stack at run-time
2 CONSTANT /CS

\ The control structures stack
MAX-LOOP-DEPTH /CS * STACK CS

\ The leaves stack
MAX-LEAVES STACK LEAVES

\ Access the elements of the control structure stack
: (CS-BEGIN-ADDR) 1 CS ST-PICK ;
: (CS-R) CS ST@ ;

\ Return how many control structures deep we are
: #CS \ ( -- n )
    CS #ST /CS / ;

\ Copy the i'th control structure onto the top of the control stack,
\ counting from 0
: CS-PICK \ ( i -- )
    1+ /CS *
    /CS
    BEGIN
	?DUP
    WHILE
	    OVER 1- CS ST-PICK CS >ST
	    1-
    REPEAT
    DROP ;

\ Drop the top control structure
: CS-DROP \ ( -- )
    /CS CS ST-NDROP ;

\ Start a new structure definition
: (CS-START) \ ( rt -- )
    TOP CS >ST        \ repeat address (here)
    CS >ST            \ elements hidden
    0 LEAVES >ST ;    \ no forward references

\ Pop the last exit point from the leave stack
: LEAVE> \ ( -- addr )
    LEAVES ST> LEAVES ST>
    SWAP 1- LEAVES >ST ;

\ End a structure definition
: (CS-END) \ ( -- )
    CS-DROP ;


\ ---------- Hook words ----------

\ Clear the control stacks if we warm-start
:NONAME
    CS ST-CLEAR
    LEAVES ST-CLEAR
    FALSE ;
HANG-ON WARMSTART-HOOK

