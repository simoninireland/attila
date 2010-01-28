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

# ----- Bootstrapping the interpreter -----

BOOTSTRAP = @BOOTSTRAP@


# ----- Main program -----

bin_PROGRAMS = bootattila


# ----- Sources -----

bootattila_SOURCES = vm.h \
	primgen.h \
	bootstrap.h \
	bootstrap.c \
	attila.c


# ----- The bootstrapped interpreter -----

BUILT_SOURCES = primitives.c
nodist_bootattila_SOURCES = primitives.c

primitives.c: bootstrap.fs c-core.fs c-fileio.fs c-io.fs
	$(BOOTSTRAP) -o primitives.c $(srcdir)/bootstrap.fs $(srcdir)/c-core.fs $(srcdir)/c-fileio.fs $(srcdir)/c-io.fs


# ----- The system sources -----

prelude = conditionals.fs \
	base.fs \
	createdoes.fs \
	interpret-compile.fs \
	variables.fs \
	constants.fs \
	values.fs \
	defer.fs \
	vm.fs \
	ascii.fs \
	stacks.fs \
	cs-stack.fs \
	loops.fs \
	counted-loops-runtime.fs \
	counted-loops.fs \
	case.fs \
	hooks.fs \
	chars.fs \
	strings.fs \
	zstrings.fs \
	scratch.fs \
	formatting.fs \
	lists.fs \
	file.fs \
	evaluate.fs \
	c.fs \
	wordlists.fs

crosscompiler = x-host.fs \
	x-host.h \
	c-image-fixedsize.fs \
	cross-locator.fs \
	cross-flat-memory-model.fs \
	cross-c.fs \
	cross-colon.fs \
	cross-createdoes.fs \
	cross-vm.fs \
	cross-generate.fs \
	cross-environments.fs

regular = c-core.fs \
	c-itil.fs \
	c-fileio.fs \
	c-io.fs \
	flat-memory-model.fs \
	itil-compilation.fs \
	compilation.fs \
	colon.fs \
	load.fs \
	executive.fs

# ----- Distribution -----

dist_pkgdata_DATA = $(prelude) $(crosscompiler) $(regular)
DISTCLEANFILES = primitives.c

