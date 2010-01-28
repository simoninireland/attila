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

\ Counted loops
\
\ Loops over an index, with the test at the bottom so that we go through
\ the loop at least once.
\
\ Loops are scoped within a single colon-definition: you can access the
\ index of the loop with the word I, but *only* within the *same*
\ colon-definition as the DO ... LOOP code itself, not from any words
\ called from that code.
\ Requires: loops.fs, conditionals.fs, counted-loops-runtime.fs

\ Start the loop
: DO
    [COMPILE] (DO)
    2 (CS-START) ; IMMEDIATE \ after the (DO) so we don't re-do the setup code

\ End the loop, with arbitrary increment for the index
: +LOOP
    [COMPILE] (+LOOP)
    [COMPILE] (?BRANCH) >BEGIN
    (CS-END) ; IMMEDIATE

\ Increment the index by one
: LOOP
    1 POSTPONE LITERAL
    [COMPILE] (+LOOP)
    [COMPILE] (?BRANCH) >BEGIN
    (CS-END) ; IMMEDIATE
\ sd: This is repetitous to facilitate cross-compilation, where a defining
\ word can't make use of other defining words
