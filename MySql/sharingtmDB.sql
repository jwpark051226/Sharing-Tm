-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------
-- Schema mydb
-- -----------------------------------------------------
-- -----------------------------------------------------
-- Schema sharingtmdb
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Schema sharingtmdb
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `sharingtmdb` DEFAULT CHARACTER SET utf8mb3 ;
USE `sharingtmdb` ;

DROP TABLE IF EXISTS Rental;
DROP TABLE IF EXISTS User;
DROP TABLE IF EXISTS RentalStation;
DROP TABLE IF EXISTS Ticket;
DROP TABLE IF EXISTS TimeMachine;

-- -----------------------------------------------------
-- Table `sharingtmdb`.`rentalstation`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `sharingtmdb`.`rentalstation` (
  `rsid` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `rs_name` VARCHAR(45) NOT NULL,
  `rs_address` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`rsid`),
  UNIQUE INDEX `rsid_UNIQUE` (`rsid` ASC) VISIBLE)
ENGINE = InnoDB
AUTO_INCREMENT = 3
DEFAULT CHARACTER SET = utf8mb3;


-- -----------------------------------------------------
-- Table `sharingtmdb`.`timemachine`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `sharingtmdb`.`timemachine` (
  `tmid` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `model` VARCHAR(6) NOT NULL,
  `current_era_point` VARCHAR(50) NOT NULL DEFAULT 'CurrentTime',
  `rsid` INT UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`tmid`),
  UNIQUE INDEX `tmid_UNIQUE` (`tmid` ASC) VISIBLE,
  INDEX `fk_TimeMachine_RentalStation1_idx` (`rsid` ASC) VISIBLE,
  CONSTRAINT `fk_TimeMachine_RentalStation1`
    FOREIGN KEY (`rsid`)
    REFERENCES `sharingtmdb`.`rentalstation` (`rsid`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb3;


-- -----------------------------------------------------
-- Table `sharingtmdb`.`user`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `sharingtmdb`.`user` (
  `userid` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `username` VARCHAR(45) NOT NULL,
  `balance` INT NOT NULL DEFAULT '0',
  PRIMARY KEY (`userid`),
  UNIQUE INDEX `userid_UNIQUE` (`userid` ASC) VISIBLE)
ENGINE = InnoDB
AUTO_INCREMENT = 4
DEFAULT CHARACTER SET = utf8mb3;


-- -----------------------------------------------------
-- Table `sharingtmdb`.`rental`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `sharingtmdb`.`rental` (
  `rentalid` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `rental_time` DATETIME NOT NULL,
  `return_time` DATETIME NULL DEFAULT NULL,
  `userid` INT UNSIGNED NULL NOT NULL,
  `tmid` INT UNSIGNED NOT NULL,
  `depart_rs` INT UNSIGNED NOT NULL,
  `arrive_rs` INT UNSIGNED NULL DEFAULT NULL,
  `destination_point` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`rentalid`),
  UNIQUE INDEX `rentalid_UNIQUE` (`rentalid` ASC) VISIBLE,
  INDEX `fk_Rental_User1_idx` (`userid` ASC) VISIBLE,
  INDEX `fk_Rental_TimeMachine1_idx` (`tmid` ASC) VISIBLE,
  INDEX `fk_Rental_RentalStation1_idx` (`depart_rs` ASC) VISIBLE,
  INDEX `fk_Rental_RentalStation2_idx` (`arrive_rs` ASC) VISIBLE,
  CONSTRAINT `fk_Rental_RentalStation1`
    FOREIGN KEY (`depart_rs`)
    REFERENCES `sharingtmdb`.`rentalstation` (`rsid`),
  CONSTRAINT `fk_Rental_RentalStation2`
    FOREIGN KEY (`arrive_rs`)
    REFERENCES `sharingtmdb`.`rentalstation` (`rsid`),
  CONSTRAINT `fk_Rental_TimeMachine1`
    FOREIGN KEY (`tmid`)
    REFERENCES `sharingtmdb`.`timemachine` (`tmid`),
  CONSTRAINT `fk_Rental_User1`
    FOREIGN KEY (`userid`)
    REFERENCES `sharingtmdb`.`user` (`userid`)
    ON DELETE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb3;


-- -----------------------------------------------------
-- Table `sharingtmdb`.`ticket`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `sharingtmdb`.`ticket` (
  `ticketid` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `purchase_time` DATETIME NOT NULL,
  `expire_time` DATETIME NOT NULL,
  `status` VARCHAR(45) NOT NULL DEFAULT 'ACTIVE',
  `userid` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`ticketid`),
  UNIQUE INDEX `ticketid_UNIQUE` (`ticketid` ASC) VISIBLE,
  INDEX `fk_Ticket_User_idx` (`userid` ASC) VISIBLE,
  CONSTRAINT `fk_Ticket_User`
    FOREIGN KEY (`userid`)
    REFERENCES `sharingtmdb`.`user` (`userid`)
    ON DELETE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 2
DEFAULT CHARACTER SET = utf8mb3;

-- 샘플 데이터 삽입
START TRANSACTION;
INSERT INTO RentalStation(rsid, rs_name, rs_address) VALUES
(1, '한국공학대학교', '경기도 시흥시 정왕동'),
(2, '정왕역', '경기도 시흥시 정왕동'),
(3, '경기과학기술대학교', '경기도 시흥시 정왕동'),
(4, '사당역', '서울특별시 동작구 사당동'),
(5, '잠실역', '서울특별시 송파구 신천동'),
(6, '시흥시청', '경기도 시흥시 장현동'),
(7, '코엑스', '서울특별시 강남구 삼성동');

INSERT INTO TimeMachine(model, rsid) VALUES
('past', 1),
('past', 3),
('past', 2),
('past', 5),
('past', 7),
('past', 5),
('past', 3),
('future', 3),
('future', 6),
('future', 7),
('future', 1),
('future', 4),
('future', 2),
('past', 4),
('past', 6),
('future', 5),
('past', 3),
('past', 2),
('future', 1),
('past', 7),
('past', 6),
('future', 4),
('future', 3),
('future', 2);
COMMIT;

SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
