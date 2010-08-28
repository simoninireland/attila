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

\ Bootstrapping system prelude

\ Simple control structures
include conditionals.fs          \ IF ... ELSE ... THEN

\ "Primitives" outside the VM core
include base.fs                  \ Derived primitives

\ Advanced compilation words
include createdoes.fs            \ CREATE ... DOES>
include interpret-compile.fs     \ INTERPRET-COMPILE
include variables.fs             \ VARIABLE
include uservariables.fs         \ USER
include constants.fs             \ CONSTANT
include values.fs                \ VALUE
include defer.fs                 \ DEFER ... IS

\ The VM
include bootstrap-vm.fs          \ Limited set of virtual machine constants 
include ascii.fs                 \ ASCII character operations

\ Data structures
include bootstrap-loops.fs       \ Simple bootstrapping loops
include stacks.fs                \ General stacks

\ Control structure support
include cs-stack.fs              \ Control structures stack
include loop-support.fs          \ Exits and other supports
\ now hide the bootstrapping loops ahead of loading the "real" ones
' BEGIN  (HIDE)
' UNTIL  (HIDE)
' WHILE  (HIDE)
' AGAIN  (HIDE)
' REPEAT (HIDE)

\ Control structures
include loops.fs                 \ BEGIN ... AGAIN
                                 \ BEGIN ... UNTIL
                                 \ BEGIN ... WHILE ... REPEAT
include counted-loops-runtime.fs \ DO ... LOOP and +LOOP
include counted-loops.fs
include case.fs                  \ CASE ... OF ... ENDOF ... ENDCASE
include hooks.fs                 \ Dynamic behaviour at strategic points

\ Data types
include chars.fs                 \ Character operations
include strings.fs               \ Counted (short) strings
include zstrings.fs              \ Null-terminated (long, C-style) strings
include scratch.fs               \ String scratch area
include formatting.fs            \ Formatted numeric output
include lists.fs                 \ Linked lists
include records.fs               \ record types with named fields

\ File management
include file.fs                  \ I/O re-direction
include evaluate.fs              \ Execute code from strings

\ Word list control
include wordlists.fs             \ Multiple word lists


