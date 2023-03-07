// This file is part of Astarte.
//
// Copyright 2023 SECO Mind Srl
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstarteDeviceSDKCSharp.Migrations
{
    public partial class AddAstarteGenericPropertyEntryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AstarteGenericProperties",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    INTERFACE_FIELD_NAME = table.Column<string>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    BsonValue = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AstarteGenericProperties", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AstarteGenericProperties");
        }
    }
}
