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

\ Tests of arithmetic operations

TESTCASES" Bit-level operations"

TESTING" Basic assumptions"
: BITS-SET? IF 1 ELSE 0 THEN ;
{ 0         BITS-SET? -> 0 }
{ 1         BITS-SET? -> 1 }
{ 1 NEGATE  BITS-SET? -> 1 }


TESTING" AND, OR, NOT, XOR, INVERT"
{ 0 0 AND -> 0 }
{ 0 1 AND -> 0 }
{ 1 0 AND -> 0 }
{ 1 1 AND -> 1 }

{ 0 0 OR -> 0 }
{ 1 0 OR -> 1 }
{ 0 1 OR -> 1 }
{ 1 1 OR -> 1 }

{ 0 0 XOR -> 0 }
{ 1 0 XOR -> 1 }
{ 0 1 XOR -> 1 }
{ 1 1 XOR -> 0 }

{ TRUE NOT  -> FALSE }
{ FALSE NOT -> TRUE }
{ 1 NOT     -> FALSE }

{ 0 INVERT 1 AND -> 1 }
{ 1 INVERT 1 AND -> 0 }

\ Cells full of zeros and full of ones
0         CONSTANT 0S
0S INVERT CONSTANT 1S

{ 0S INVERT -> 1S }
{ 1S INVERT -> 0S }

{ 0S 0S AND -> 0S }
{ 0S 1S AND -> 0S }
{ 1S 0S AND -> 0S }
{ 1S 1S AND -> 1S }

{ 0S 0S OR -> 0S }
{ 1S 0S OR -> 1S }
{ 0S 1S OR -> 1S }
{ 1S 1S OR -> 1S }

{ 0S 0S XOR -> 0S }
{ 1S 0S XOR -> 1S }
{ 0S 1S XOR -> 1S }
{ 1S 1S XOR -> 0S }


TESTING" Bit-level arithmetic operations"

\ Cell with only its most significant bit set
1S 1 RSHIFT INVERT CONSTANT MSB

{ MSB BITS-SET? -> 1 }

{ 0s 2*       -> 0S }
{ 1 2*        -> 2 }
{ 1000 2*     -> 2000 }
{ 1S 2* 1 XOR -> 1S }
{ MSB 2*      -> 0S }

{ 0s 2/          -> 0S }
{ 1 2/           -> 0 }
{ 2 2/           -> 1 }
{ 2000 2/        -> 1000 }
{ 1S 1 XOR 2/    -> 1S }
{ MSB 2/ MSB AND -> MSB }

{ 1 0 LSHIFT        -> 1 }
{ 1 1 LSHIFT        -> 2 }
{ 1 2 LSHIFT        -> 4 }
{ 1S 1 LSHIFT 1 XOR -> 1S }
{ MSB 1 LSHIFT      -> 0 }

{ 1 0 RSHIFT           -> 1 }
{ 1 1 RSHIFT           -> 0 }
{ 2 1 RSHIFT           -> 1 }
{ 4 2 RSHIFT           -> 1 }
{ MSB 1 RSHIFT MSB AND -> 0 }
{ MSB 1 RSHIFT 2*      -> MSB }


TESTING" Arithmetic comparisons"

\ Maximum and minimum values, assuming 2's complement arithmetic
0 INVERT                 CONSTANT MAX-UINT
0 INVERT 1 RSHIFT        CONSTANT MAX-INT
0 INVERT 1 RSHIFT INVERT CONSTANT MIN-INT
0 INVERT 1 RSHIFT        CONSTANT MID-UINT
0 INVERT 1 RSHIFT INVERT CONSTANT MID-UINT+1

{ 0 0=        -> TRUE }
{ 1 0=        -> FALSE }
{ 2 0=        -> FALSE }
{ -1 0=       -> FALSE }
{ MAX-UINT 0= -> FALSE }
{ MIN-INT 0=  -> FALSE }
{ MAX-INT 0=  -> FALSE }

{ 0 0 =   -> TRUE }
{ 1 1 =   -> TRUE }
{ -1 -1 = -> TRUE }
{ 1 0 =   -> FALSE }
{ -1 0 =  -> FALSE }
{ 0 1 =   -> FALSE }
{ 0 -1 =  -> FALSE }

{ 0 0<       -> FALSE }
{ -1 0<      -> TRUE }
{ MIN-INT 0< -> TRUE }
{ 1 0<       -> FALSE }
{ MAX-INT 0< -> FALSE }

{ 0 0<=  -> TRUE }
{ 1 0<=  -> FALSE }
{ -1 0<= -> TRUE }
{ 0 0>=  -> TRUE }
{ 1 0>=  -> TRUE }
{ -1 0>= -> FALSE }

{ 0 1 <             -> TRUE }
{ 1 2 <             -> TRUE }
{ -1 0 <            -> TRUE }
{ -1 1 <            -> TRUE }
{ MIN-INT 0 <       -> TRUE }
{ MIN-INT MAX-INT < -> TRUE }
{ 0 MAX-INT <       -> TRUE }
{ 0 0 <             -> FALSE }
{ 1 1 <             -> FALSE }
{ 1 0 <             -> FALSE }
{ 2 1 <             -> FALSE }
{ 0 -1 <            -> FALSE }
{ 1 -1 <            -> FALSE }
{ 0 MIN-INT <       -> FALSE }
{ MAX-INT MIN-INT < -> FALSE }
{ MAX-INT 0 <       -> FALSE }

{ 0 1 >             -> FALSE }
{ 1 2 >             -> FALSE }
{ -1 0 >            -> FALSE }
{ -1 1 >            -> FALSE }
{ MIN-INT 0 >       -> FALSE }
{ MIN-INT MAX-INT > -> FALSE }
{ 0 MAX-INT >       -> FALSE }
{ 0 0 >             -> FALSE }
{ 1 1 >             -> FALSE }
{ 1 0 >             -> TRUE }
{ 2 1 >             -> TRUE }
{ 0 -1 >            -> TRUE }
{ 1 -1 >            -> TRUE }
{ 0 MIN-INT >       -> TRUE }
{ MAX-INT MIN-INT > -> TRUE }
{ MAX-INT 0 >       -> TRUE }

\ { 0 1 U<               -> TRUE }
\ { 1 2 U<               -> TRUE }
\ { 0 MID-UINT U<        -> TRUE }
\ { 0 MAX-UINT U<        -> TRUE }
\ { MID-UINT MAX-UINT U< -> TRUE }
\ { 0 0 U<               -> FALSE }
\ { 1 1 U<               -> FALSE }
\ { 1 0 U<               -> FALSE }
\ { 2 1 U<               -> FALSE }
\ { MID-UINT 0 U<        -> FALSE }
\ { MAX-UINT 0 U<        -> FALSE }
\ { MAX-UINT MID-UINT U< -> FALSE }

{ 0 1 MIN             -> 0 }
{ 1 2 MIN             -> 1 }
{ -1 0 MIN            -> -1 }
{ -1 1 MIN            -> -1 }
{ MIN-INT 0 MIN       -> MIN-INT }
{ MIN-INT MAX-INT MIN -> MIN-INT }
{ 0 MAX-INT MIN       -> 0 }
{ 0 0 MIN             -> 0 }
{ 1 1 MIN             -> 1 }
{ 1 0 MIN             -> 0 }
{ 2 1 MIN             -> 1 }
{ 0 -1 MIN            -> -1 }
{ 1 -1 MIN            -> -1 }
{ 0 MIN-INT MIN       -> MIN-INT }
{ MAX-INT MIN-INT MIN -> MIN-INT }
{ MAX-INT 0 MIN       -> 0 }

{ 0 1 MAX             -> 1 }
{ 1 2 MAX             -> 2 }
{ -1 0 MAX            -> 0 }
{ -1 1 MAX            -> 1 }
{ MIN-INT 0 MAX       -> 0 }
{ MIN-INT MAX-INT MAX -> MAX-INT }
{ 0 MAX-INT MAX       -> MAX-INT }
{ 0 0 MAX             -> 0 }
{ 1 1 MAX             -> 1 }
{ 1 0 MAX             -> 1 }
{ 2 1 MAX             -> 2 }
{ 0 -1 MAX            -> 0 }
{ 1 -1 MAX            -> 1 }
{ 0 MIN-INT MAX       -> 0 }
{ MAX-INT MIN-INT MAX -> MAX-INT }
{ MAX-INT 0 MAX       -> MAX-INT }


TESTING" Basic arithmetic operations"

{ 0 5 +        -> 5 }
{ 5 0 +        -> 5 }
{ 0 -5 +       -> -5 }
{ -5 0 +       -> -5 }
{ 1 2 +        -> 3 }
{ 1 -2 +       -> -1 }
{ -1 2 +       -> 1 }
{ -1 -2 +      -> -3 }
{ -1 1 +       -> 0 }
{ MID-UINT 1 + -> MID-UINT+1 }

{ 0 5 -          -> -5 }
{ 5 0 -          -> 5 }
{ 0 -5 -         -> 5 }
{ -5 0 -         -> -5 }
{ 1 2 -          -> -1 }
{ 1 -2 -         -> 3 }
{ -1 2 -         -> -3 }
{ -1 -2 -        -> 1 }
{ 0 1 -          -> -1 }
{ MID-UINT+1 1 - -> MID-UINT }

{ 0 1+        -> 1 }
{ -1 1+       -> 0 }
{ 1 1+        -> 2 }
{ MID-UINT 1+ -> MID-UINT+1 }

{ 2 1-          -> 1 }
{ 1 1-          -> 0 }
{ 0 1-          -> -1 }
{ MID-UINT+1 1- -> MID-UINT }

{ 0 NEGATE  -> 0 }
{ 1 NEGATE  -> -1 }
{ -1 NEGATE -> 1 }
{ 2 NEGATE  -> -2 }
{ -2 NEGATE -> 2 }

{ 0 ABS       -> 0 }
{ 1 ABS       -> 1 }
{ -1 ABS      -> 1 }
{ MIN-INT ABS -> MID-UINT+1 }

{ 0 0 *   -> 0 }
{ 0 1 *   -> 0 }
{ 1 0 *   -> 0 }
{ 1 2 *   -> 2 }
{ 2 1 *   -> 2 }
{ 3 3 *   -> 9 }
{ -3 3 *  -> -9 }
{ 3 -3 *  -> -9 }
{ -3 -3 * -> 9 }

{ MID-UINT+1 1 RSHIFT 2 *               -> MID-UINT+1 }
{ MID-UINT+1 2 RSHIFT 4 *               -> MID-UINT+1 }
{ MID-UINT+1 1 RSHIFT MID-UINT+1 OR 2 * -> MID-UINT+1 }
