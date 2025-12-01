-- 给日报表增加请假状态字段
-- LeaveStatus: NULL=不请假, 'off'=全天请假, 'AM leave'=上午请假, 'PM leave'=下午请假
ALTER TABLE DailyReports
ADD LeaveStatus NVARCHAR(20) NULL;
