// $Id: arithmetic.c,v 1.2 2007/05/18 19:02:12 sd Exp $

// This file is part of Attila, a multi-targeted threaded interpreter
// Copyright (c) 2007, UCD Dublin. All rights reserved.
//
// Attila is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// Attila is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

// Arithmetic primitives
//
// Single-precision arithmetic. This includes normal arithmetic operations,
// comparisons, constants and bit-level operations

#include "attila.h"


// ---------- Constants ----------

// Largest signed number available
// PRIMITIVE MAX-INT ( -- n )
void
max_int() {
  CELL m = INT_MAX;
  PUSH_CELL(m);
}


// Largessmallest signed number available
// PRIMITIVE MIN-INT ( -- n )
void
min_int() {
  CELL m = INT_MIN;
  PUSH_CELL(m);
}


// ---------- Arithmetic ----------

// Negate the top number
// PRIMITIVE NEGATE ( n -- -n )
void
negate() {
  CELL n;
  POP_CELL(n);
  n *= -1;
  PUSH_CELL(n);
}


// Make the top number positive
// PRIMITIVE ABS ( n -- n' )
void
absolute() {
  CELL n;
  POP_CELL(n);
  if(n < 0) n *= -1;
  PUSH_CELL(n);
}


// Integer addition
// PRIMITIVE + ( a b -- a+b )
void
add() {
  CELL a, b, c;

  POP_CELL(b);   POP_CELL(a);
  c = a + b;
  PUSH_CELL(c);
}


// Fast increment
// PRIMITIVE 1+ ( a -- a+1 )
void
inc() {
  CELLPTR a;
  TOPPTR_CELL(a);
  (*a)++;
}


// Fast decrement
// PRIMITIVE 1- ( a -- a=1 )
void
dec() {
  CELLPTR a;
  TOPPTR_CELL(a);
  (*a)--;
}


// Fast multiply by two, for masking
// PRIMITIVE 2* ( a -- 2*a )
void
two_times() {
  CELLPTR a;
  TOPPTR_CELL(a);
  (*a) = (*a) << 1;
}


// Fast divide by two, for masking
// PRIMITIVE 2/ ( a -- a/2 )
void
two_divide() {
  CELLPTR a;
  TOPPTR_CELL(a);
  (*a) = (*a) >> 1;
}


// Increment by a cell
// PRIMITIVE CELL+ ( addr -- addr' )
void
inc_cell() {
  CELL a;
  POP_CELL(a);
  a += CELL_SIZE;
  PUSH_CELL(a);
}


// Integer subtraction
// PRIMITIVE - ( a b -- a-b )
void
subtract() {
  CELL a, b, c;

  POP_CELL(b);   POP_CELL(a);
  c = a - b;
  PUSH_CELL(c);
}


// Integer multiplication
// PRIMITIVE * ( a b -- a*b )
void
multiply() {
  CELL a, b, c;

  POP_CELL(b);   POP_CELL(a);
  c = a * b;
  PUSH_CELL(c);
}


// Divide a by b leaving the remainder and quotient
// PRIMITIVE /MOD ( a b -- a%b a/b )
void
div_mod() {
  CELL a, b, q, r;

  POP_CELL(b);   POP_CELL(a);
  q= (CELL) (a / b);
  r = (CELL) (a % b );
  PUSH_CELL(r);   PUSH_CELL(q);
}


// Leave the divisor and remainder on the stack
// PRIMITIVE */MOD ( a b c -- (a*b)%c (a*b)/c )
void
star_div_mod() {
  CELL a, b, c, i, q, r;

  POP_CELL(c);  POP_CELL(b);   POP_CELL(a);
  i = a * b;
  q = i / c;   r = i % c;
  PUSH_CELL(r);   PUSH_CELL(q);
}


// Maximum
// PRIMITIVE MAX ( a b -- max )
void
mmax() {
  CELL a, b, c;

  POP_CELL(b);   POP_CELL(a);
  c = (a > b) ? a : b;
  PUSH_CELL(c);
}


// Minimum
// PRIMITIVE MIN ( a b -- min )
void
mmin() {
  CELL a, b, c;

 POP_CELL(b);   POP_CELL(a);
  c = (a > b) ? b : a;
  PUSH_CELL(c);
}


// ---------- Bit-level operators ----------

// Bitwise and
// PRIMITIVE AND ( a b -- a&b )
void
bit_and() {
  CELL a, b, c;

  POP_CELL(b);   POP_CELL(a);
  c = a & b;
  PUSH_CELL(c);
}


// Bitwise or
// PRIMITIVE OR ( a b -- a|b )
void
bit_or() {
  CELL a, b, c;

  POP_CELL(b);   POP_CELL(a);
  c = a | b;
  PUSH_CELL(c);
}


// Bitwise exclusive-or
// PRIMITIVE XOR ( a b -- a^b )
void
bit_xor() {
  CELL a, b, c;

  POP_CELL(b);   POP_CELL(a);
  c = a ^ b;
  PUSH_CELL(c);
}


// Bitwise not
// PRIMITIVE INVERT ( a -- ~a )
void
bit_not() {
  CELL a, b;

  POP_CELL(a);
  b = ~a;
  PUSH_CELL(b);
}


// Bitwise left-shift
// PRIMITIVE LSHIFT ( a n -- a<<n )
void
bit_shift_left() {
  CELL a, n, c;

  POP_CELL(n);   POP_CELL(a);
  c = a << n;
  PUSH_CELL(c);
}


// Bitwise right-shift
// PRIMITIVE RSHIFT ( a n -- a>>n )
void
bit_shift_right() {
  CELL a, n, c;

  POP_CELL(n);   POP_CELL(a);
  c = a >> n;
  PUSH_CELL(c);
}


// ---------- Logical tests ----------

// Test greater-than-or-equal-to
// PRIMITIVE >= ( a b -- f )
void
test_gte() {
  CELL a, b, c;

  POP_CELL(b);   POP_CELL(a);
  c = (a >= b);
  PUSH_CELL(c);
}


// Test greater-than
// PRIMITIVE > ( a b -- f )
void
test_gt() {
  CELL a, b, c;

  POP_CELL(b);   POP_CELL(a);
  c = (a > b);
  PUSH_CELL(c);
}


// Test less-than-or-equal-to
// PRIMITIVE <= ( a b -- f )
void
test_lte() {
  CELL a, b, c;

  POP_CELL(b);   POP_CELL(a);
  c = (a <= b);
  PUSH_CELL(c);
}


// Test less-than
// PRIMITIVE < ( a b -- f )
void
test_lt() {
  CELL a, b, c;

  POP_CELL(b);   POP_CELL(a);
  c = (a < b);
  PUSH_CELL(c);
}


// Test equal
// PRIMITIVE = ( a b -- f )
void
test_eq() {
  CELL a, b, c;

  POP_CELL(b);   POP_CELL(a);
  c = (a == b);
  PUSH_CELL(c);
}


// Test inequal
// PRIMITIVE <> ( a b -- f )
void
test_neq() {
  CELL a, b, c;

  POP_CELL(b);   POP_CELL(a);
  c = (a != b);
  PUSH_CELL(c);
}


// Test for zero
// PRIMITIVE 0= ( n -- f )
void
test_eqzero() {
  CELL a, f;

  POP_CELL(a);
  f = (a == 0);
  PUSH_CELL(f);
}


// Test for non-zero
// PRIMITIVE 0<> ( n -- f )
void
test_neqzero() {
  CELL a, f;

  POP_CELL(a);
  f = (a != 0);
  PUSH_CELL(f);
}


// Test for less than zero
// PRIMITIVE 0< ( n -- f )
void
test_ltzero() {
  CELL n, f;

  POP_CELL(n);
  f = (n < 0);
  PUSH_CELL(f);
}


// Test for greater than zero
// PRIMITIVE 0> ( n -- f )
void
test_gtzero() {
  CELL n, f;

  POP_CELL(n);
  f = (n > 0);
  PUSH_CELL(f);
}


// Test whether a lies between b and c
// PRIMITIVE WITHIN ( a b c -- f )
void
within() {
  CELL a, b, c, f;

  POP_CELL(c);   POP_CELL(b);   POP_CELL(a);
  f = (a <= b) && (a < c);
  PUSH_CELL(f);
}
