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

\ Tests of ASCII character testing

TESTCASES" ASCII character encoding"

TESTING" Basic constants and classifications"

{ BL WS? -> TRUE }
{ NL WS? -> TRUE }
{ TB WS? -> TRUE }
{ [CHAR] A WS? -> FALSE }


TESTING" Encodings and conversions"

{ [CHAR] A UC? -> TRUE }
{ [CHAR] Z UC? -> TRUE }
{ [CHAR] a UC? -> FALSE }
{ [CHAR] z UC? -> FALSE }

{ [CHAR] A 1- UC? -> FALSE }
{ [CHAR] a 1- LC? -> FALSE }
{ [CHAR] Z 1+ UC? -> FALSE }
{ [CHAR] z 1+ lC? -> FALSE }

{ [CHAR] A >UC -> [CHAR] A }
{ [CHAR] a >UC -> [CHAR] A }
{ [CHAR] Z >LC -> [CHAR] z }
{ [CHAR] z >LC -> [CHAR] z }
{ [CHAR] 1 >UC -> [CHAR] 1 }
{ [CHAR] 1 >LC -> [CHAR] 1 }






