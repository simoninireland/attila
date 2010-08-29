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

\ Stack handling tests

.( Duplicating)
{ 1 DUP -> 1 1 }
{ 1 2 DUP -> 1 2 2 }

.( Swapping)
{ 1 2 SWAP -> 2 1 }

.( Dropping)
{ 1 DROP -> }

.( Rot'ting and rolling)
{ 1 2 3 ROT -> 3 1 2 }
{ 1 2 3 -ROT -> 2 3 1 }
{ 1 0 ROLL -> 1 NOOP }
{ 1 2 1 ROLL -> 1 2 SWAP }
{ 1 2 3 2 ROLL -> 1 2 3 ROT }
