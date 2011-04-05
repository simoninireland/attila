// $Id$

// This file is part of Attila, a retargetable threaded interpreter
// Copyright (c) 2007--2011, Simon Dobson <simon.dobson@computer.org>.
// All rights reserved.
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

// Standard type mappings

#ifndef STDC_TYPES_STANDARD
#define STDC_TYPES_STANDARD

#include <stdlib.h>

// ---------- Type mapping ----------

// Base types
typedef void VOID;
typedef long CELL;
typedef unsigned long UCELL;
typedef long long DOUBLE_CELL;
typedef VOID *XT;
typedef unsigned char BYTE;
typedef unsigned char CHARACTER;

// Derived pointer types
typedef CELL *CELLPTR;
typedef XT *XTPTR;
typedef BYTE *BYTEPTR;
typedef CHARACTER *CHARACTERPTR;
typedef VOID (*PRIMITIVE)( XT _xt );


// ---------- Helper macros and functions ----------

// Primitive calling macro
#define CALL( p ) p(_xt)

// Fatal error re-start
#define DIE( msg ) (printf("FATAL: %s\n", msg), longjmp(env, 0), (CELL) 0)

#endif
