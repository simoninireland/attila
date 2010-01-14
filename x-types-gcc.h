// $Id$

// Type mappings for host gcc compiler

#ifndef X_TYPES_GCC_H
#define X_TYPES_GCC_H

// Base types
typedef void VOID;
typedef long CELL;
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

#endif
