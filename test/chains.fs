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

\ Tests of chains of values

TESTCASES" Chains"

TESTING" Basic chain operations"

VARIABLE CHAIN-BASE

\ data block for testing
DATA CHAIN-DATA-BLOCK 4 3 2 1 0 , , , , ,

\ one link
0 CHAIN-BASE !
CHAIN-DATA-BLOCK 5 CELLS CHAIN-BASE ADD-LINK-TO-CHAIN
{ CHAIN-BASE NEXT-LINK-IN-CHAIN DATA-LINK-IN-CHAIN NIP -> 5 CELLS }
{ CHAIN-BASE NEXT-LINK-IN-CHAIN DATA-LINK-IN-CHAIN DROP @ -> 0 }
{ CHAIN-BASE NEXT-LINK-IN-CHAIN DATA-LINK-IN-CHAIN DROP 4 CELLS + @ -> 4 }

\ two links
CHAIN-DATA-BLOCK 4 CELLS CHAIN-BASE ADD-LINK-TO-CHAIN
{ CHAIN-BASE NEXT-LINK-IN-CHAIN DATA-LINK-IN-CHAIN NIP -> 5 CELLS }
{ CHAIN-BASE NEXT-LINK-IN-CHAIN DATA-LINK-IN-CHAIN DROP 4 CELLS + @ -> 4 }
{ CHAIN-BASE NEXT-LINK-IN-CHAIN NEXT-LINK-IN-CHAIN DATA-LINK-IN-CHAIN NIP -> 4 CELLS }
{ CHAIN-BASE NEXT-LINK-IN-CHAIN NEXT-LINK-IN-CHAIN DATA-LINK-IN-CHAIN DROP 3 CELLS + @ -> 3 }

\ three links
CHAIN-DATA-BLOCK 1 CELLS CHAIN-BASE ADD-LINK-TO-CHAIN
{ CHAIN-BASE NEXT-LINK-IN-CHAIN DATA-LINK-IN-CHAIN NIP -> 5 CELLS }
{ CHAIN-BASE NEXT-LINK-IN-CHAIN NEXT-LINK-IN-CHAIN NEXT-LINK-IN-CHAIN DATA-LINK-IN-CHAIN NIP -> 1 CELLS }
{ CHAIN-BASE NEXT-LINK-IN-CHAIN NEXT-LINK-IN-CHAIN DATA-LINK-IN-CHAIN DROP @ -> 0 }


TESTING" Chains and strings"

0 CHAIN-BASE !
S" Hello" CHAIN-BASE ADD-LINK-TO-CHAIN
{ CHAIN-BASE NEXT-LINK-IN-CHAIN DATA-LINK-IN-CHAIN S" Hello" S= -> TRUE }

S" world!" CHAIN-BASE ADD-LINK-TO-CHAIN
{ CHAIN-BASE NEXT-LINK-IN-CHAIN DATA-LINK-IN-CHAIN S" Hello" S= -> TRUE }
{ CHAIN-BASE NEXT-LINK-IN-CHAIN NEXT-LINK-IN-CHAIN DATA-LINK-IN-CHAIN S" world!" S= -> TRUE }


TESTING" Retrieving a complete chain"

{ CHAIN-BASE ALL-DATA-LINKS-IN-CHAIN -ROT 2DROP -ROT 2DROP -> 2 }
{ CHAIN-BASE ALL-DATA-LINKS-IN-CHAIN DROP 2SWAP 2DROP S" world!" S= -> TRUE }
{ CHAIN-BASE ALL-DATA-LINKS-IN-CHAIN DROP 2DROP       S" Hello"  S= -> TRUE }
