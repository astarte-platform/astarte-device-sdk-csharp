// This file is part of Astarte.
//
// Copyright 2023 SECO Mind Srl
//
// SPDX-License-Identifier: Apache-2.0

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstarteDeviceSDKCSharp.Migrations
{
    public partial class AddInterfaceMajor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InterfaceMajor",
                table: "AstarteGenericProperties",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InterfaceMajor",
                table: "AstarteGenericProperties");
        }
    }
}
