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

\ Test the core aspcets of the system. This test suite is intended to
\ ensure Attila is functioning correctly after cross-compilation

TEST-CAMPAIGN" Core system tests"

testcases test/stack.fs
testcases test/arithmetic.fs
testcases test/words.fs
testcases test/allot.fs
testcases test/ascii.fs     \ assuming system uses an ASCII encoding
testcases test/strings.fs
testcases test/formatting.fs
testcases test/loops.fs
testcases test/conditional-compilation.fs
testcases test/hooks.fs
testcases test/parser.fs

TEST-CAMPAIGN-REPORT
