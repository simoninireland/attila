\ $Id: x-host.fs.in 801 2011-03-12 18:04:22Z sd $

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

\ Driver file for host cross-compilation
\
\ This file can re-create the development image for Attila, and serves
\ as a model for other drivers for differemt targets

include cross-compiler/cross-target.fs

\ Set up host development system's include path, as set
\ when the system was installed
S" target/host:@datadir@:@datadir@/target/host" SET-CROSS-COMPILER-TARGET-INCLUDE-PATH

\ Other paths will be relative to this
S" host"                 SET-CROSS-COMPILER-TARGET-NAME
S" vm.fs"                SET-CROSS-COMPILER-TARGET-VM-FILE
S" c-main-interactive.h" SET-CROSS-COMPILER-TARGET-MAIN-FILE
S" attila.c"             SET-CROSS-COMPILER-OUTPUT-FILE

\ Run the standard image builder
include cross-compiler/x-standard.fs

