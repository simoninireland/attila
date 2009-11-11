# $Id: Makefile.am,v 1.13 2007/05/25 13:13:26 sd Exp $

# This file is part of Attila, a multi-targeted threaded interpreter
# Copyright (c) 2007, UCD Dublin. All rights reserved.
#
# Attila is free software; you can redistribute it and/or
# modify it under the terms of the GNU General Public License
# as published by the Free Software Foundation; either version 2
# of the License, or (at your option) any later version.
#
# Attila is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

# ----- Primitives table builder script, filled-in by configure -----

BOOTSTRAP = @BUILDBOOTSTRAP@


# ----- Main program -----

bin_PROGRAMS = attila


# ----- Sources -----

attila_SOURCES = attila.h \
	memory.c util.c \
	stack.c dictionary.c bootstrapping.c \
	vm.c arithmetic.c interactive.c vocabulary.c \
	initial.c \
	attila.c
attila_LDDADD = @READLINELIB@


# ----- The generated primitives table -----

BUILT_SOURCES = bootstrap.c
nodist_attila_SOURCES = bootstrap.c

bootstrap.c: vm.c arithmetic.c interactive.c vocabulary.c
	$(BOOTSTRAP) bootstrap.c $(srcdir)/vm.c $(srcdir)/arithmetic.c $(srcdir)/interactive.c $(srcdir)/vocabulary.c


# ----- The prelude and other source files -----

prelude = vm.fs colon.fs base.fs createdoes.fs variables.fs \
	interpret-compile.fs \
	defer.fs stacks.fs cs-stack.fs \
	conditionals.fs case.fs loops.fs counted-loops.fs \
	strings.fs scratch.fs \
	formatting.fs \
	vocabularies.fs \
	dynamic-memory.fs \
	locals-base.fs locals.fs ans-locals.fs \
	prelude.fs


# ----- Distribution -----

dist_pkgdata_DATA = $(prelude)
info_TEXINFOS = attila.texi
DISTCLEANFILES = bootstrap.c attila.pdf

