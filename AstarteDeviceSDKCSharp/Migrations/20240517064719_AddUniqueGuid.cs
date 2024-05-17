// This file is part of Astarte.
//
// Copyright 2024 SECO Mind Srl
//
// SPDX-License-Identifier: Apache-2.0
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstarteDeviceSDKCSharp.Migrations
{
    public partial class AddUniqueGuid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "Index_Guid",
                table: "AstarteFailedMessages");

            migrationBuilder.CreateIndex(
                name: "Index_Guid",
                table: "AstarteFailedMessages",
                column: "guid",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "Index_Guid",
                table: "AstarteFailedMessages");

            migrationBuilder.CreateIndex(
                name: "Index_Guid",
                table: "AstarteFailedMessages",
                column: "guid");
        }
    }
}
