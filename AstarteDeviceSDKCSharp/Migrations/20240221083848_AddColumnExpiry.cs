// This file is part of Astarte.
//
// Copyright 2024 SECO Mind Srl
//
// SPDX-License-Identifier: Apache-2.0
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstarteDeviceSDKCSharp.Migrations
{
    public partial class AddColumnExpiry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "absolute_expiry",
                table: "AstarteFailedMessages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "absolute_expiry",
                table: "AstarteFailedMessages");
        }
    }
}
