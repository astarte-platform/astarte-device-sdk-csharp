// This file is part of Astarte.
//
// Copyright 2024 SECO Mind Srl
//
// SPDX-License-Identifier: Apache-2.0
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstarteDeviceSDKCSharp.Migrations
{
    public partial class AddColumnProcessed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Processed",
                table: "AstarteFailedMessages",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Processed",
                table: "AstarteFailedMessages");
        }
    }
}
