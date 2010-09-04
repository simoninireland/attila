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

\ Loop support words

\ ---------- Support words ----------

\ Compile the address of the start of the current structure
: >BEGIN \ ( -- )
    (CS-BEGIN-ADDR) JUMP-BACKWARD ;

\ Compile a placeholder to the end of the current structure
: >END \ ( -- )
    LEAVES ST>
    JUMP-FORWARD LEAVES >ST
    1+ LEAVES >ST ;

\ Patch all the jumps to the end of the structure with the current TOP address
: ENDS> ( -- )    
    LEAVES ST>
    BEGIN
	?DUP
    WHILE
	    LEAVES ST> JUMP-HERE
	    1-
    REPEAT ;


\ ---------- Exits ----------

\ Compile code to tidy-up the return stack ahead of an early exit
\ sd: Should we compile this somehow, rather than repeating the >Rs so much?
\ sd: We should also abstract the way the return stack is handled
: (LEAVE)
    (CS-R)
    BEGIN
	?DUP
    WHILE
	    [COMPILE] RDROP
	    1-
    REPEAT ;

\ Escape from the current loop, jumping to the end
: LEAVE \ ( -- )
    (LEAVE)
    [COMPILE] (BRANCH) >END ; IMMEDIATE
    
\ Return from this word immediately, unwinding any loops
\ sd: Will need re-coding when we do exceptions, to thread back up the stack
:  EXIT \ ( -- )
    #CS
    BEGIN
	?DUP
    WHILE
	    DUP 1- CS-PICK
	    (LEAVE)
	    CS-DROP
	    1-
    REPEAT
    NEXT, ; IMMEDIATE



