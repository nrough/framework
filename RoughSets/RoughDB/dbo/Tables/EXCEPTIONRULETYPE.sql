﻿CREATE TABLE [dbo].[EXCEPTIONRULETYPE] (
    [EXCEPTIONTYPEID] INT           NOT NULL,
    [NAME]            NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_EXCEPTIONRULETYPE] PRIMARY KEY CLUSTERED ([EXCEPTIONTYPEID] ASC)
);
