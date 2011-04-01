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

\ "Standard" cross-compiler driver to build a "normal", interactive VM image
\
\ This driver will create an image suitable for most purposes, on
\ a development box. Specifically it can be used to cross-compile
\ another image.


\ ---------- Phase 0: Cross-compiler loading ----------
\ - Load the cross-compiler
\ - Give it the appropriate view of the intended target VM

\ Load the cross-compiler
include cross-compiler/cross-compiler.fs

.( Initialising cross-compiler view of target VM...)
<cross-compiler also cross-compiler
CROSS-COMPILER-TARGET-VM-FILE included
cross-compiler>


\ ---------- Phase 1: Minimal image population ----------
\ - Set up the image
\ - Load really basic primitives, inner interpreter

.( Initialising cross-compiler target image...)
CROSS-COMPILER-TARGET-NAME [cross] initialise-image

.( Initialising image with really basic primitives...)
<target
C: USERVAR uservar ( n -- addr )
  addr = (CELL) &image[n];
;C

include c-core.fs
include c-flat-memory-model-runtime.fs
include c-itil.fs
target>


\ ---------- Phase 2: Cross-compiler set-up ----------
\ - Load control structures
\ - Load hooks support

.( Initialising cross-compiler support...)
<cross-compiler also cross-compiler
\ loop support
include cs-stack.fs
include loop-support.fs

\ loop cross-compilation
include conditionals.fs
include loops.fs
include counted-loops.fs

\ CREATE...DOES>
: START-DEFINITION ;
: END-DEFINITION ;
include createdoes.fs

\ ITIL words
<cross
include words-itil-common.fs
include words-itil-named.fs
cross>

\ hooks support
<cross
include chain.fs
include hooks-runtime.fs
cross>
include hooks.fs
cross-compiler>

.( Initialising include path management...)
<cross-compiler
\ include path set-up -- messy, messy...
: ADD-SYSTEM-INCLUDE-PATH ( addr n -- )
    [CROSS-COMPILER] ['] SYSTEM-INCLUDE-PATH >BODY ADD-LINK-TO-CHAIN ;
: ADD-SYSTEM-INCLUDE-PATHS ( addr n -- )
    [CHAR] : SPLIT
    BEGIN
	?DUP 0>
    WHILE
	    ROT [CROSS-COMPILER] ADD-SYSTEM-INCLUDE-PATH
	    1-
    REPEAT ;
cross-compiler>


\ ---------- Phase 3: Populate the image ----------
\ - Load other capabilities into image

.( Populating target image...)
<target
\ basic facilities
include counted-loops-runtime.fs

\ character, memory, threading and word models
include ascii.fs
include flat-memory-model.fs
include itil-compilation.fs
include words-itil-common.fs
include words-itil-named.fs

\ continuations
include c-continuations-runtime.fs

\ hooks and chains
include chain.fs
include hooks-runtime.fs
include system-hooks.fs

\ terminal and file i/o
include c-fileio.fs
include type.fs
include io.fs

\ starting and re-starting
include warm.fs
include abort.fs

\ parsing and colon-compilation
include parser.fs
include compilation.fs
include colon.fs

\ code loading
include file.fs
include load.fs
include include.fs

\ interactive executive
include executive.fs
include startup.fs
target>


\ ---------- Phase 4: Image finalisation ----------

.( Aligning image...)
[CROSS] ALIGN

.( Finalising target image...)
[CROSS] FINALISE-IMAGE

.( Image created successfully)
