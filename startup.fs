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

\ Normal start-up
\
\ This word can be changed, but probably never will be unles the
\ system is running without hooks or if a particular turnkey
\ behaviour is wanted from the get-go.
\
\ As a general rule, START should never actually be executed from within
\ a session. It might actually be better to have it as a :NONAME.


\ Start an Attila session
: START ( -- )
    \ re-set the restart vector, so if we restart again we'll
    \ do a warm-start
    ['] WARM (START) XT!
    
    \ run the start-up hook
    STARTUP-HOOK RUN-HOOK DROP

    \ warm-start the system 
    WARM ;

