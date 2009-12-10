include comments.fs
\ ... and now we can parse comments, we can begin ... :-)

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

\ Standard system prelude
\
\ This file loads and compiles the desired system from source. This is
\ a "standard" prelude in the sense of loading a "complete" language:
\ it can be changed as required, as long as you understand the
\ dependencies that exist between some of the source files (clearly
\ indicated in the comments for the files in the distribution).

\ "Primitives" outside the VM core, mainly wrappers for "real" primitives
include base.fs
include conditionals.fs          \ IF ... ELSE ... THEN

\ Advanced compilation words
include createdoes.fs            \ CREATE ... DOES>
include interpret-compile.fs     \ INTERPRET-COMPILE
include variables.fs             \ VARIABLE, CONSTANT, VALUE and USER

\ The VM
include vm.fs

\ Compilation helpers
include defer.fs                 \ DEFER ... IS

\ Data structures
include stacks.fs                \ General stacks

\ Control structures
include cs-stack.fs              \ Control structures stack
include loops.fs                 \ BEGIN ... AGAIN
                                  \ BEGIN ... UNTIL
                                  \ BEGIN ... WHILE ... REPEAT
include counted-loops.fs         \ DO ... LOOP
                                  \ DO ... +LOOP
include case.fs                  \ CASE ... OF ... ENDOF ... ENDCASE

\ Data types
include strings.fs               \ Character and string operations
include scratch.fs               \ String scratch area
include formatting.fs            \ Formatted numeric output
include lists.fs                 \ Linked lists

\ Word list control
include wordlists.fs             \ Multiple word lists

\ Dynamic memory
\ include dynamic-memory.fs        \ ALLOCATE, FREE, RESIZE

\ Local variables
\ include locals-base.fs           \ base definitions
\ include ans-locals.fs            \ LOCALS| ... | for symbolic local definitions
\ include locals.fs                \ stack-comment-style locals

