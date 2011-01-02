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

\ Bootstrapping system prelude. This contains enough to bootstrap the
\ system and run the cross-compiler. See prelude.fs for a fuller
\ language definition.

\ Simple control structures
include conditionals.fs
include bootstrap-loops.fs

\ "Primitives" outside the VM core
include base.fs

\ Advanced compilation words
include createdoes.fs
include interpret-compile.fs
include variables.fs
include uservariables.fs
include constants.fs
include values.fs
include defer.fs

\ The VM
include bootstrap-vm.fs
include ascii.fs

\ Data structures
include stacks.fs

\ Control structure support
include cs-stack.fs
include loop-support.fs
\ now hide the bootstrapping loops ahead of loading the "real" ones
' BEGIN  (HIDE)
' UNTIL  (HIDE)
' WHILE  (HIDE)
' AGAIN  (HIDE)
' REPEAT (HIDE)

\ Control structures
include loops.fs
include counted-loops-runtime.fs 
include counted-loops.fs
include case.fs
include conditional-compilation.fs

\ Data types
include chars.fs
include strings.fs
include zstrings.fs
include scratch.fs
include formatting.fs
include lists.fs
include records.fs

\ File management
include file.fs

\ Word list control
: ADD-TO-HOOK ! ;
include wordlists.fs


