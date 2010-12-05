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
\ Ths START word performs the actions needed to start the system from
\ scratch. By default this is to load the standard prelude and then
\ warm-start the interpreter, setting the future restart point to
\ the executive.
\
\ As a general rule, this word should never be executed from within
\ a session.

\ Start an Attila session
: START ( -- )
    \ re-set the interpreter
    (RESET)
    
    \ re-set the restart vector, so that WARM will jump
    \ to it when called
    ['] OUTER (START) XT!

    \ annouce the system
    S" Attila v.1.0" TYPE
    
    \ include the standard prelude
    S" prelude.fs" INCLUDED

    \ warm-start 
    WARM ;

