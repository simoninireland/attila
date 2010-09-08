include comments.fs
\ ... and now we can parse comments, we can begin ... :-)

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

\ Standard system prelude
\
\ This file loads and compiles the desired system from source. This is
\ a "standard" prelude in the sense of loading a "complete" language:
\ it can be changed as required, as long as you understand the
\ dependencies that exist between some of the source files (clearly
\ indicated in the comments for the files in the distribution).

\ Simple control structures
include conditionals.fs              \ IF ... ELSE ... THEN
include bootstrap-loops.fs           \ Simple loops, without proper control

\ "Primitives" outside the VM core
include base.fs                      \ Derived primitives

\ Advanced compilation words
include createdoes.fs                \ CREATE ... DOES>
include interpret-compile.fs         \ INTERPRET-COMPILE
include variables.fs                 \ VARIABLE
include uservariables.fs             \ USER
include constants.fs                 \ CONSTANT
include stringconstants.fs           \ SCONSTANT
include values.fs                    \ VALUE ... TO
include defer.fs                     \ DEFER ... IS

\ The VM
include vm.fs                        \ Virtual machine structures and constants 
include ascii.fs                     \ ASCII operations

\ Data structures
include stacks.fs                    \ General stacks

\ Control structures
include cs-stack.fs                  \ Control structures stack
include loop-support.fs              \ Exits and other supports
\ hide the simple loops, replace with "proper" ones
' BEGIN  (HIDE)
' UNTIL  (HIDE)
' WHILE  (HIDE)
' AGAIN  (HIDE)
' REPEAT (HIDE)
include loops.fs                     \ BEGIN ... AGAIN
                                     \ BEGIN ... UNTIL
                                     \ BEGIN ... WHILE ... REPEAT
include counted-loops-runtime.fs     \ DO ... LOOP and +LOOP
include counted-loops.fs
include case.fs                      \ CASE ... OF ... ENDOF ... ENDCASE
include hooks.fs                     \ Dynamic behaviour at strategic points
include conditional-compilation.fs   \ [IF] ... [ELSE] ... [THEN]

\ Data types
include chars.fs                     \ Character operations
include strings.fs                   \ Counted (short) strings
include zstrings.fs                  \ Null-terminated (long, C-style) strings
include scratch.fs                   \ String scratch area
include formatting.fs                \ Formatted numeric output
include lists.fs                     \ Linked lists
include records.fs                   \ Record types with named fields

\ File management
include file.fs                      \ I/O re-direction
include evaluate.fs                  \ Execute code from strings
\ include file-templates.fs        \ File templates

\ Foreign language support
include c.fs                         \ C definitions (check for definitions)

\ Word list control
include wordlists.fs                 \ Multiple word lists

\ Dynamic memory
include dynamic-memory.fs            \ ALLOCATE, FREE, RESIZE

\ Compilation and debugging support
include decompiler.fs                \ Word decompilation

\ Local variables
\ include locals-base.fs           \ base definitions
\ include ans-locals.fs            \ LOCALS| ... | for symbolic local definitions
\ include locals.fs                \ stack-comment-style locals

