\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007--2011, Simon Dobson <simon.dobson@computer.org>.
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

\ Hooks
\
\ Hooks collect together a list of words that can be changed dynamically.
\ The idea is that a hook collects together the words that should happen
\ a particular "strategic" point -- for example when a file is loaded,
\ or when a word is defined -- and lets different parts of the system
\ specify some behaviour to be performed then.
\
\ Each word hung on a hook should have stack comment ( ... -- ... f ),
\ leaving a flag on the stack indicating whether the hook should keep
\ running words. A flag of TRUE stops execution: this should be read as
\ "this hook call has now been fully dealt with". The flag returned by the
\ last word run is left on the stack after running the hook. This means
\ that hooks can be used to "try" operations, with the hook succeeding
\ as soon as a word returns TRUE, and a flag of FALSE meas that there
\ is still "work to do" in some sense. Hooked words are run oldest-to-youngest.
\
\ Hooks are dynamic, so the structure is maintained in data memory.
\ The run-time component is in hooks-runtime.fs

\ Create a hook with the given name
: (HOOK) ( addr n -- )
    (DATA)
    0 , ;

\ Create a hook
: HOOK ( "name" -- )
    PARSE-WORD (HOOK) ;

\ Hang the given xt on the named hook. This is the preferred way to
\ hang words, and works with the cross-compiler
: HANG-ON ( xt "name" -- )
    ' >BODY ADD-TO-HOOK ;
