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

\ Tests of formatted string output

TESTCASES" Formatted string output"

TESTING" Character holding"

\ Holding single characters
{ CLEAR-SCRATCH [CHAR] A HOLD
                SCRATCH> SWAP C@ -> 1 [CHAR] A }
{ CLEAR-SCRATCH [CHAR] A HOLD
                [CHAR] B HOLD 
                SCRATCH> SWAP DUP C@ SWAP 1+ C@ -> 2 [CHAR] A [CHAR] B }


TESTING" String holding"

\ Holding strings
{ CLEAR-SCRATCH S" AB" SHOLD
                SCRATCH> SWAP DUP C@ SWAP 1+ C@ -> 2 [CHAR] A [CHAR] B }


TESTING" Formatting numbers"

\ Digit and multi-digit formatting
{  1 <# #   #> S" 1"  S= -> TRUE }
{  1 <# #S  #> S" 1"  S= -> TRUE }
{ 12 <# # # #> S" 12" S= -> TRUE }
{ 12 <# #S  #> S" 12" S= -> TRUE }

\ Held characters go in the right place
{ 12 <# #S [CHAR] . HOLD #> S" .12" S= -> TRUE }

\ Sign handling
{ 12        NUMBER> S" 12"  S= -> TRUE }
{ 12 NEGATE NUMBER> S" -12" S= -> TRUE }

\ Base handling
{ 10 HEX NUMBER> DECIMAL S" A" S= -> TRUE }
