// $Id: util.c,v 1.3 2007/05/22 08:31:43 sd Exp $

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

// Some utility functions

#include "attila.h"


// Convert a TIL-format string to a C-format string, which must
// then be free'd
char *til_string_to_c_string( char *str, int n ) {
  char *buf = (char *) calloc(sizeof(char), n + 1);
  strncpy(buf, str, n);   buf[n] = '\0';
  return buf;
}
