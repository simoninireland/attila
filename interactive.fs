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

\ Words needed to control interactive mode
\
\ Typically these words are hung on the appropriate hooks during
\ cross-compilation.


\ ---------- Handy words to be hung on the startup hook ----------

\ Announce the system
: ANNOUNCE
    S" Attila v." TYPE
    VM-VERSION TYPE
    32 ( SPACE ) EMIT
    S" (" TYPE
    VM-TARGET-NAME TYPE
    S" /" TYPE
    VM-RUNTIME TYPE
    S" )" TYPE
    0 ;

\ Load the standard prelude
: LOAD-PRELUDE
    S" prelude.fs" INCLUDED
    0 ;


\ ---------- Handy words for the startup/warmstart hooks ----------

\ Reset the elements of an interactive environment
: INTERACTIVE
    ['] OUTER            EXECUTIVE XT!  \ interactive outer executive
    INTERPRETATION-STATE STATE       !  \ interpreting
    10                   BASE        !  \ decimal
    0                    TRACE       !  \ not debugging
    -1                   >IN         !  \ TIB needs a refill
    0                    TIB @      C!  \ no data in TIB
    0 ;

