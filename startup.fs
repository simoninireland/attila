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

\ Normal interactive start-up
\
\ This START word performs the actions needed to start the system from
\ scratch. By default this is to load the standard prelude and then
\ warm-start the interpreter, setting the future restart point to
\ the executive.
\
\ As a general rule, this word should never be executed from within
\ a session.

\ Handy words to be hung on the startup hook
: ANNOUNCE     S" Attila v.1.0" TYPE     0 ;
: LOAD-PRELUDE S" prelude.fs"   INCLUDED 0 ;

\ Handy words for the warmstart hook
: INTERACTIVE
    ['] OUTER            EXECUTIVE XT!  \ interactive outer executive
    INTERPRETATION-STATE STATE       !  \ interpreting
    10                   BASE        !  \ decimal
    0                    TRACE       !  \ not debugging
    -1                   >IN         !  \ TIB needs a refill
    0                    TIB @      C!  \ no data in TIB
    0 ;

\ Start an Attila session
: START ( -- )
    \ re-set the restart vector, so if we restart again we'll
    \ do a warm-start
    ['] WARM (START) XT!
    
    \ run the start-up hook
    STARTUP-HOOK RUN-HOOK DROP

    \ warm-start the system 
    WARM ;

