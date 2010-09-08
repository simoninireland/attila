# $Id$

# This file is part of Attila, a retargetable threaded interpreter
# Copyright (c) 2007--2010, Simon Dobson <simon.dobson@computer.org>.
# All rights reserved.
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

# ----- Main programs -----

bin_PROGRAMS = bootattila attila


# ----- Supporting tools for bootstrapping -----

dist_bin_SCRIPTS = tools/primgen \
	tools/crosscompile.fs


# ----- Sources -----

# The bootstrapping sources
bootattila_SOURCES = vm.h \
	primgen.h \
	bootstrap.fs \
	bootstrap.h \
	bootstrap.c \
	bootattila.c \
	bootstrap-prelude.fs \
	bootstrap-vm.fs

# The generated primitives file
nodist_bootattila_SOURCES = primitives.c

# The cross-compiled sources
attila_SOURCES = x-host.h \
	x-host.fs

# The generated VM file
nodist_attila_SOURCES = attila.c


# ----- The bootstrapped interpreter primitives file -----

primitives.c: bootstrap.fs c-core.fs c-fileio.fs c-io.fs
	$(PERL) $(srcdir)/tools/primgen -o primitives.c $(srcdir)/bootstrap.fs $(srcdir)/c-core.fs $(srcdir)/c-fileio.fs $(srcdir)/c-io.fs


# ----- The cross-compiled interpreter VM -----

attila.c: bootattila
	cd $(srcdir) && $(abs_builddir)/bootattila <tools/crosscompile.fs


# ----- The system sources -----

# All the Forth code intended for normal use
forth = comments.fs \
	conditionals.fs \
	base.fs \
	createdoes.fs \
	interpret-compile.fs \
	variables.fs \
	uservariables.fs \
	constants.fs \
	values.fs \
	defer.fs \
	vm.fs \
	ascii.fs \
	bootstrap-loops.fs \
	stacks.fs \
	cs-stack.fs \
	loop-support.fs \
	loops.fs \
	counted-loops-runtime.fs \
	counted-loops.fs \
	case.fs \
	hooks.fs \
	conditional-compilation.fs \
	chars.fs \
	strings.fs \
	zstrings.fs \
	scratch.fs \
	formatting.fs \
	lists.fs \
	records.fs \
	file.fs \
	evaluate.fs \
	c.fs \
	wordlists.fs \
	dynamic-memory.fs \
	decompiler.fs \
	funit.fs

# The cross-compiler
crosscompiler = x-host.fs \
	x-host.h \
	cross-compiler/cross-compiler.fs \
	cross-compiler/cross-locator.fs \
	cross-compiler/cross-flat-memory-model.fs \
	cross-compiler/cross-image-fixedsize.fs \
	cross-compiler/cross-c.fs \
	cross-compiler/cross-colon.fs \
	cross-compiler/cross-c-vm.fs \
	cross-compiler/cross-c-generate.fs \
	cross-compiler/cross-environments.fs

# The system sources, used to cross-compile a basic virtual machine
regular = c-core.fs \
	c-itil.fs \
	itil.fs \
	c-fileio.fs \
	c-io.fs \
	flat-memory-model.fs \
	itil-compilation.fs \
	compilation.fs \
	colon.fs \
	load.fs \
	executive.fs \
	startup.fs

# Tests
test = test/core-system-tests.fs \
	test/arithmetic.fs \
	test/stack.fs \
	test/loops.fs \
	test/allot.fs \
	test/conditional-compilation.fs

# ----- Distribution -----

dist_pkgdata_DATA = $(forth) $(crosscompiler) $(regular) $(test)
DISTCLEANFILES = primitives.c

