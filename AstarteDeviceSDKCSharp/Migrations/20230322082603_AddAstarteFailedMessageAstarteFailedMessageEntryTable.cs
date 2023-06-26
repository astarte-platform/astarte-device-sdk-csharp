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
    public partial class AddAstarteFailedMessageAstarteFailedMessageEntryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AstarteFailedMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Qos = table.Column<int>(type: "INTEGER", nullable: false),
                    Payload = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Topic = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AstarteFailedMessages", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AstarteFailedMessages");
        }
    }
}
